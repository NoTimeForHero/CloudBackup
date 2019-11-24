<script>
import { onMount } from 'svelte';
export let settings;

let logs = [];
let alert = null;

$: styleForLine = (line) => {
    if (!line[1]) return;
    switch (line[1]) {
        case 'TRACE':
        case 'DEBUG':
            return 'log-debug';
        case 'INFO':
            return 'log-info';            
        case 'WARN':
            return 'log-warn';
        case 'FATAL':
        case 'ERROR':
            return 'log-error';            
    }
    return '';
}

const loadLog = async() => {
    try {
        alert = { class: 'info', message: 'Загрузка данных с сервера...' };        
        const data = await fetch(settings.apiUrl + '/logs').then(x => x.json());
        logs = data.map(x => x.split("|"));
        alert = null;
    } catch (ex) {
        alert = { class: 'danger', message: ex }			
    }
}

const btnRefresh = async() => {
    alert("Обновлено!");
}

onMount(loadLog);
</script>

<style>
.log-debug {
    color: lightslategrey;
}

.log-error {
    color: darkred;
}

.log-warn {
    color: darkgoldenrod;
}
</style>

<div class="row mt-1">

	{#if alert}
	<div class="col-12">
		<div class="alert {'alert-' + alert.class}" role="alert">{alert.message}</div>	
	</div>
	{/if}

    <div class="col-12">
        <button class="m-2 btn btn-secondary" on:click={loadLog}>Обновить логи</button>
    </div>

    <div class="col-12 logs m-2">
        <table class="table">
            <tr class="thead-dark">
                <th>DateTime</th>
                <th>LogLevel</th>
                <th>Logger</th>
                <th>Message</th>
            </tr>
            {#each logs as line}
            <tr class="{styleForLine(line)}">
                {#each line as col}<td>{col}</td>{/each}
            </tr>
            {/each}
        </table>
    </div>
</div>