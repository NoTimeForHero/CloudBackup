import { SocketClient } from './socket';

export type BootstrapStyle = 'primary'|'secondary'|'success'|'danger'|'warning'|'info'|'light'|'dark';
export type LogLevel = 'TRACE'|'DEBUG'|'INFO'|'WARN'|'ERROR'|'FATAL';
export type HttpMethod = 'CONNECT'|'DELETE'|'GET'|'HEAD'|'OPTIONS'|'PATCH'|'POST'|'PUT'|'TRACE';

export interface Settings {
  appName: string,
  apiUrl: string
  isService: boolean
}

export interface ExtendedWindow extends Window {
  settings?: Settings,
  client?: SocketClient
}