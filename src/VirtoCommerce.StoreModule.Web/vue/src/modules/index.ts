import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule } from "@vc-shell/framework";
import type { App } from "vue";
import { registerStoreSearch } from "./search";

const appModule = defineAppModule({
  blades,
  locales,
});

export default {
  install(app: App) {
    appModule.install(app);
    // stores in the Admin Shell's global search
    registerStoreSearch(app);
  },
};
