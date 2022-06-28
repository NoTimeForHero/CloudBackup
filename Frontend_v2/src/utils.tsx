import { Inputs, useEffect } from 'preact/compat';

export type CXArgument = string|Record<string,boolean>;
export const cx = (...args: CXArgument[]) : string => {
  return args.map((element) => {
    if (typeof element === 'string') return element;
    if (typeof element === 'object') {
      return Object.entries(element)
        .filter(([_, show]) => show)
        .map(([name]) => name)
        .join(' ');
    }
    console.warn(element, typeof element);
    throw new Error('Unknown type!');
  }).join(' ');
}

export const wait = (timeout: number) : Promise<void> =>
  new Promise((resolve) => setTimeout(resolve, timeout));

export const useAsyncEffect = (effect: () => Promise<void>, deps?: Inputs|undefined) =>
  useEffect(() => {
    // noinspection JSIgnoredPromiseFromCall
    effect();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

interface useLoadableOptions {
  setLoading?: (arg: boolean) => void,
  setError?: (arg: unknown) => void,
  consoleError?: boolean,
  onError?: (ex: unknown) => void,
}

export const wrapLoadable = async (callback: () => Promise<any>, options?: useLoadableOptions) => {
  try {
    options?.setError?.call(null, undefined);
    options?.setLoading?.call(null, true);
    await callback();
  } catch (ex) {
    if (options?.consoleError ?? true) console.error(ex);
    options?.setError?.call(null, ex);
    options?.onError?.call(null, ex);
  } finally {
    options?.setLoading?.call(null, false);
  }
}
export const useLoadable = (callback: () => Promise<any>, options?: useLoadableOptions, deps?: Inputs | undefined) =>
  useAsyncEffect(() => wrapLoadable(callback, options), deps ?? []);


export const renderError = (rawError: any) : string|JSX.Element|undefined => {
  if (rawError == null || typeof rawError === 'undefined') return undefined;
  if (typeof rawError === 'string') return rawError;
  if (rawError instanceof Error) return <div>
    <strong>{rawError.name}: </strong>
    {rawError.message}
  </div>
  return JSON.stringify(rawError);
};