<script>
import { onMount } from 'svelte';
export let settings;

let plugins = [];
let alert = null;

const initialize = async() => {
    try {
        alert = { class: 'info', message: 'Загрузка данных с сервера...' };        
        const data = await fetch(settings.apiUrl + '/plugins').then(x => x.json());
        plugins = data;
        alert = null;
    } catch (ex) {
        alert = { class: 'danger', message: ex }			
    }
}

onMount(initialize);
</script>

<style>
</style>

<div class="row mt-1">

	{#if alert}
	<div class="col-12">
		<div class="alert {'alert-' + alert.class}" role="alert">{alert.message}</div>	
	</div>
	{/if}

    <div class="col-12 logs m-2">
        <table class="table">
            <tr class="thead-dark">
                <th>ID</th>
                <th>Название</th>
                <th>Описание</th>
            </tr>
            {#each plugins as plugin}
                <tr>
                    <td>{plugin.Id}</td>
                    <td>{plugin.Name}</td>
                    <td>{plugin.Description}</td>
                </tr>
            {/each}
        </table>
    </div>
</div>