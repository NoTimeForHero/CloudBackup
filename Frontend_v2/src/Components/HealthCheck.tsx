import { useSockets } from '../socket';
import { FC, useMemo, useState } from 'preact/compat';
import { useAsyncEffect } from '../utils';

enum Type {
  online,
  offline
}

const URLS : Record<Type, string> = {
  [Type.online]: "/icons/cloud_online.png",
  [Type.offline]: "/icons/cloud_offline.png",
}

const states : Record<Type, string|undefined> = {
  [Type.online]: undefined,
  [Type.offline]: undefined,
}

const makeImage = (url: string) => fetch(url).then(x => x.blob()).then(x => URL.createObjectURL(x));

const loadImages = async() => {
  await new Promise((resolve) => setTimeout(resolve, 300));
  const promises = Object.values(URLS).map(makeImage);
  const blobs = await Promise.all(promises);
  // TODO: Написать динамически и читабельно
  states[Type.online] = blobs[0];
  states[Type.offline] = blobs[1];
}

const HealthCheck : FC = () => {
  const [loaded,setLoaded] = useState(false);
  const [active, setActive] = useState<boolean>(false);

  const imageType = useMemo(() => active ? Type.online : Type.offline, [active]);
  useSockets('any', (active: boolean) => setActive(active));

  useAsyncEffect(async() => {
    await loadImages();
    setLoaded(true);
  }, []);

  if (!loaded) return <span>...</span>;
  return <img src={states[imageType]} alt="" />;
}

export default HealthCheck;