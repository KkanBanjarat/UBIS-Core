// src/auth/msalConfig.ts
import { PublicClientApplication ,LogLevel} from '@azure/msal-browser'
import type { Configuration } from '@azure/msal-browser'

const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID}`,
    redirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'sessionStorage',
  },
  system: {
    loggerOptions: {
      loggerCallback: (_level: LogLevel, _message: string) => {},
      logLevel: LogLevel.Warning,
    },
  },
}

export const msalInstance = new PublicClientApplication(msalConfig)

let initPromise: Promise<Awaited<ReturnType<typeof msalInstance.handleRedirectPromise>>> | null = null

export async function ensureMsalInitialized() {
  if (!initPromise) {
    initPromise = (async () => {
      await msalInstance.initialize()
      return msalInstance.handleRedirectPromise()
    })()
  }
  return initPromise
}
