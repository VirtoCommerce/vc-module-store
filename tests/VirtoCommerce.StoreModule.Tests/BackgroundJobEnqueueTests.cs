using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.StoreModule.Core.Events;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Handlers;
using VirtoCommerce.StoreModule.Data.Jobs;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests
{
    // Any other test class that enqueues through the static BackgroundJob facade must join this collection:
    // the facade has no reset API (Initialize rejects null), so Dispose leaves a DISPOSED provider behind in
    // the static, and a class racing this one would see ObjectDisposedException from it.
    [Collection(nameof(BackgroundJobEnqueueTests))]
    public class BackgroundJobEnqueueTests
    {
        [Fact]
        public async Task LogChanges_EnqueuesOneJobWithOneLogPerChangedEntry()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new LogChangesChangedEventHandler(Mock.Of<IChangeLogService>());

            var message = new StoreChangedEvent(
            [
                new GenericChangedEntry<Store>(new Store { Id = "store1" }, new Store { Id = "store1" }, EntryState.Modified),
                new GenericChangedEntry<Store>(new Store { Id = "store2" }, new Store { Id = "store2" }, EntryState.Added),
            ]);

            //Act
            await handler.Handle(message);

            //Assert
            var payload = Assert.IsType<LogEntityChangesJobPayload>(capture.Payload);
            Assert.Equal(1, capture.EnqueueCount);
            Assert.Equal(["store1", "store2"], payload.OperationLogs.Select(x => x.ObjectId));
            Assert.Equal([EntryState.Modified, EntryState.Added], payload.OperationLogs.Select(x => x.OperationType));
        }

        [Fact]
        public async Task LogEntityChangesJobHandler_SavesThePayloadLogs()
        {
            //Arrange
            var operationLogs = new[] { AbstractTypeFactory<OperationLog>.TryCreateInstance() };
            var changeLogServiceMock = new Mock<IChangeLogService>();

            var handler = new LogEntityChangesJobHandler(changeLogServiceMock.Object);

            //Act
            await handler.Execute(new LogEntityChangesJobPayload { OperationLogs = operationLogs }, context: null,
                TestContext.Current.CancellationToken);

            //Assert
            changeLogServiceMock.Verify(x => x.SaveChangesAsync(operationLogs), Times.Once);
        }

        [Fact]
        public async Task UserVerificationEmail_EnqueuesTheUserVerbatim()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new SendStoreUserVerificationEmailHandler(Mock.Of<IStoreNotificationSender>());

            var user = new ApplicationUser { Id = "user1", StoreId = "store1" };

            //Act
            await handler.Handle(new UserVerificationEmailEvent(user));

            //Assert
            // The store check lives in the job, not the enqueue: the handler enqueues unconditionally, as before.
            var payload = Assert.IsType<SendUserEmailVerificationJobPayload>(capture.Payload);
            Assert.Equal(1, capture.EnqueueCount);
            Assert.Same(user, payload.User);
        }

        [Fact]
        public async Task SendUserEmailVerificationJobHandler_UserBelongsToStore_Sends()
        {
            //Arrange
            var senderMock = new Mock<IStoreNotificationSender>();
            var handler = new SendUserEmailVerificationJobHandler(senderMock.Object);

            var user = new ApplicationUser { Id = "user1", StoreId = "store1" };

            //Act
            await handler.Execute(new SendUserEmailVerificationJobPayload { User = user }, context: null,
                TestContext.Current.CancellationToken);

            //Assert
            senderMock.Verify(x => x.SendUserEmailVerificationAsync(user), Times.Once);
        }

        [Fact]
        public async Task SendUserEmailVerificationJobHandler_UserWithoutStore_SendsNothing()
        {
            //Arrange
            var senderMock = new Mock<IStoreNotificationSender>();
            var handler = new SendUserEmailVerificationJobHandler(senderMock.Object);

            var user = new ApplicationUser { Id = "user1" };

            //Act
            await handler.Execute(new SendUserEmailVerificationJobPayload { User = user }, context: null,
                TestContext.Current.CancellationToken);

            //Assert
            senderMock.Verify(x => x.SendUserEmailVerificationAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        // Captures what a handler enqueued through the static BackgroundJob facade. IBackgroundJob is registered
        // Scoped here exactly as the engine module registers it, so this also proves the facade's per-call scope
        // resolves it - the handlers themselves are root-resolved and must never hold it.
        private sealed class EnqueueCapture : IDisposable
        {
            private readonly ServiceProvider _provider;

            public EnqueueCapture()
            {
                BackgroundJobMock
                    .Setup(x => x.Enqueue<LogEntityChangesJobHandler>(It.IsAny<object>(), It.IsAny<EnqueueOptions>(), It.IsAny<CancellationToken>()))
                    .Callback<object, EnqueueOptions, CancellationToken>((payload, _, _) => Capture(payload))
                    .ReturnsAsync("job-id");

                BackgroundJobMock
                    .Setup(x => x.Enqueue<SendUserEmailVerificationJobHandler>(It.IsAny<object>(), It.IsAny<EnqueueOptions>(), It.IsAny<CancellationToken>()))
                    .Callback<object, EnqueueOptions, CancellationToken>((payload, _, _) => Capture(payload))
                    .ReturnsAsync("job-id");

                var services = new ServiceCollection();
                services.AddScoped(_ => BackgroundJobMock.Object);
                _provider = services.BuildServiceProvider(validateScopes: true);

                BackgroundJob.Initialize(_provider);
            }

            public Mock<IBackgroundJob> BackgroundJobMock { get; } = new();

            public object Payload { get; private set; }

            public int EnqueueCount { get; private set; }

            public void Dispose()
            {
                _provider.Dispose();
            }

            private void Capture(object payload)
            {
                Payload = payload;
                EnqueueCount++;
            }
        }
    }
}
