// src/auth/msalConfig.ts
import { PublicClientApplication, LogLevel } from "@azure/msal-browser";
import type { Configuration } from "@azure/msal-browser";

const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID}`,
    redirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: "sessionStorage",
  },
  system: {
    loggerOptions: {
      loggerCallback: (_level: LogLevel, _message: string) => {},
      logLevel: LogLevel.Warning,
    },
  },
};

export const msalInstance = new PublicClientApplication(msalConfig);

let initPromise: Promise<
  Awaited<ReturnType<typeof msalInstance.handleRedirectPromise>>
> | null = null;

export async function ensureMsalInitialized() {
  if (!initPromise) {
    initPromise = (async () => {
      await msalInstance.initialize();
      try {
        return await msalInstance.handleRedirectPromise();
      } catch (err: any) {
        if (err?.errorCode === "no_token_request_cache_error") {
          // Cache เก่าค้างจาก Redirect ที่ไม่สมบูรณ์ — เคลียร์แล้วถือว่าไม่มี Redirect เกิดขึ้น
          console.warn("MSAL: เจอ Cache ค้างจาก Redirect เก่า เคลียร์และไปต่อ");
          sessionStorage.clear();
          return null;
        }
        throw err;
      }
    })().catch((err) => {
      initPromise = null; // ล้มเหลวแล้วต้องไม่ Cache ไว้ ให้เรียกใหม่ได้
      throw err;
    });
  }
  return initPromise;
}
