<script>
	export let settings;
	import { onMount, onDestroy } from 'svelte';
	import bytes from 'bytes';

	let timerUpdate = null;
	let jobs = [];
	let alert = {
		class: 'info',
		message: 'Загрузка данных с сервера...'
	};

	$: calcPercent = (state) => Math.floor(state.current / state.total * 100);
	$: isBytes = (state, key) => !state.isBytes ? state[key] : bytes(state[key]);

	const btnStart = async(job) => {
		const jobName = encodeURIComponent(job.Key.Name);
		const res = await fetch(settings.apiUrl + '/start/' + jobName, {method: 'POST'});
		document.location.reload();
	}

	async function updateJobs() {
		try {
			jobs = await fetch(settings.apiUrl).then(x => x.json())
			alert = null;
		} catch (ex) {
			jobs = [];
			alert = { class: 'danger', message: ex }			
		}
	}

	onMount(()=> {
		updateJobs();
		timerUpdate = setInterval(updateJobs, 2000);
	});

	onDestroy(()=> {
		clearInterval(timerUpdate);
	});
</script>

<style>
.center {
	text-align: center;
}
</style>

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
					{#if !job.State.inProgress}
					<button class="btn btn-success" on:click={() => btnStart(job)}>
						<i class="fa fa-play" aria-hidden="true"></i>&nbsp;Запустить задачу							
					</button>
					{:else}
					<div class="center">
						<h4 style="text-align: center">{job.State.status}</h4>
						<div class="progress">
							<div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar"
								aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: {calcPercent(job.State)}%"></div>
						</div>		
						{#if job.State.total > 0}
						<div class="mt-1">
							{isBytes(job.State, 'current')} / {isBytes(job.State, 'total')}
						</div>						
						{/if}
					</div>						
					{/if}
				</div>
			</div>
		</div>
	{/each}
</div>