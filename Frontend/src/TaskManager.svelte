<script>
	export let settings;
	import { onMount } from 'svelte';

	let jobs = [];
	let alert = {
		class: 'info',
		message: 'Загрузка данных с сервера...'
	};

	onMount(async()=> {
		try {
			const data = await fetch(settings.apiUrl).then(x => x.json());
			alert = null;
			jobs = jobs.concat(data);
			jobs = jobs; // Svelte need it to refresh array
		} catch (ex) {
			alert = { class: 'danger', message: ex }
		}
	});
</script>

<style></style>

<div class="row mt-3 d-flex justify-content-center">

	{#if alert}
	<div class="col-12">
		<div class="alert {'alert-' + alert.class}" role="alert">{alert.message}</div>	
	</div>
	{/if}

	{#each jobs as job}
		<div class="col-sm-5 m-2">
			<div class="card">
				<div class="card-header">
					Задача: <strong>{job.Key.Name}</strong>
				</div>
				<div class="card-body">
					<button class="btn btn-success">
						<i class="fa fa-play" aria-hidden="true"></i>&nbsp;Запустить задачу							
					</button>
					{JSON.stringify(job.State)}
				</div>
			</div>
		</div>
	{/each}
</div>