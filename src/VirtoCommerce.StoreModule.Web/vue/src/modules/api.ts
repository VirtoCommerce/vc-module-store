// Small same-origin fetch helper for platform/module endpoints that have no generated client here
// (catalog, core, SEO duplicates). Cookie auth works because the host is same-origin.
async function http<T>(method: string, url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method,
    credentials: "same-origin",
    headers: body === undefined ? { Accept: "application/json" } : { Accept: "application/json", "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  if (!res.ok) {
    throw new Error(`${method} ${url}: ${res.status} ${(await res.text()).slice(0, 300)}`);
  }
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

export const api = {
  get: <T>(url: string) => http<T>("GET", url),
  post: <T>(url: string, body?: unknown) => http<T>("POST", url, body ?? {}),
  put: <T>(url: string, body?: unknown) => http<T>("PUT", url, body ?? {}),
  delete: <T>(url: string) => http<T>("DELETE", url),
};

export const qs = (params: Record<string, string | string[] | undefined>) =>
  Object.entries(params)
    .flatMap(([k, v]) => {
      if (v === undefined) return [];
      const values = Array.isArray(v) ? v : [v];
      return values.map((x) => `${k}=${encodeURIComponent(x)}`);
    })
    .join("&");

export interface Option {
  id: string;
  title: string;
}

export const toOptions = (values: unknown[] | undefined): Option[] =>
  (values ?? []).map((x) => ({ id: String(x), title: String(x) }));
