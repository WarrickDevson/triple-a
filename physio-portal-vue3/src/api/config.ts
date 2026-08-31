// In production builds, strictly use the configured VITE_API_BASE_URL (https://mytriplea.co.za).
// In local development over mobile tunnels (ngrok/lan), use relative path so Vite proxies to local .NET API.
const isDev = import.meta.env.DEV
const isNonLocalDev = isDev && typeof window !== 'undefined' && window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1'

export const API_BASE_URL = isNonLocalDev ? '' : (import.meta.env.VITE_API_BASE_URL ?? 'https://mytriplea.co.za')
