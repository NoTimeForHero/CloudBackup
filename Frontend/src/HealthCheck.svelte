<script>
import { useSockets } from './socket.js';
import { onMount } from 'svelte';

const URLS = {
    online: "/icons/cloud_online.png",
    offline: "/icons/cloud_offline.png",
}

let isReady = false;
let states = {
    online: undefined,
    offline: undefined,
}

let isActive = false;
$: state = isActive ? states.online : states.offline;

const makeImage = (url) => fetch(url).then(x => x.blob()).then(x => URL.createObjectURL(x));

onMount(async() => {
    await new Promise((resolve) => setTimeout(resolve, 300));
    const promises = Object.values(URLS).map(makeImage);
    const blobs = await Promise.all(promises);
    // TODO: Написать динамически и читабельно
    states = { online: blobs[0], offline: blobs[1] };
    isReady = true;
})

useSockets('any', (is) => isActive = is);
</script>

<div>
    {#if isReady}
        <img src={state} alt="" />
    {:else}
        ...
    {/if}
</div>