import { ExtendedWindow, HttpMethod, LogLevel } from '../types';
import { JobList, Plugin, SystemResponse } from './types';

const getApiUrl = (target: string) => (window as ExtendedWindow).settings!.apiUrl + '/' + target;

export type LogLine = [
  date: string,
  level: LogLevel,
  logger: string,
  message: string
]

const rawFetch = <T,>(target: string, method?: HttpMethod) =>
  fetch(getApiUrl(target), { method }).then(x => x.json()) as Promise<T>;


export const getLogs = async() => {
  const lines = await rawFetch<string[]>('logs');
  const logs = lines.map(x => x.split("|"));
  return logs as LogLine[];
}

export const getJobs = () => rawFetch<JobList>('jobs');

export const getPlugins = () => rawFetch<Plugin[]>('plugins');

export const makeReload = () =>
  rawFetch<SystemResponse>('reload');

export const makeShutdown = () =>
  rawFetch<SystemResponse>('shutdown', 'DELETE');

export const runJob = (name: string, chain?: boolean) => {
  let url = `jobs/start/${name}`;
  if (chain) url += '?runAfter=true';
  return rawFetch<unknown>(url)
}