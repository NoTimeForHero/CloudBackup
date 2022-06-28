import { ExtendedWindow, HttpMethod, LogLevel } from '../types';

const getApiUrl = (target: string) => (window as ExtendedWindow).settings!.apiUrl + '/' + target;

export type LogLine = [
  date: string,
  level: LogLevel,
  logger: string,
  message: string
]

export interface Plugin {
  Id: string,
  Name: string,
  Description: string,
  Url: string,
  Author: string,
  Version: string
}

export interface SystemResponse {
  Message: string
}

const rawFetch = <T,>(target: string, method?: HttpMethod) =>
  fetch(getApiUrl(target), { method }).then(x => x.json()) as Promise<T>;


export const getLogs = async() => {
  const lines = await rawFetch<string[]>('logs');
  const logs = lines.map(x => x.split("|"));
  return logs as LogLine[];
}

export const getPlugins = () => rawFetch<Plugin[]>('plugins');

export const makeReload = () =>
  rawFetch<SystemResponse>('reload');

export const makeShutdown = () =>
  rawFetch<SystemResponse>('shutdown', 'DELETE');