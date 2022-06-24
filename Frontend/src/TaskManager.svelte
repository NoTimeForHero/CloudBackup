<script>
	export let settings;
	import { useSockets } from './socket.js';
	import { onMount, onDestroy } from 'svelte';
	import bytes from 'bytes';

	let timerUpdate = null;
	let jobs = {};
	let alert = {
		class: 'info',
		message: 'Загрузка данных с сервера...'
	};

	$: calcPercent = (state) => Math.floor(state.current / state.total * 100);
	$: isBytes = (state, key) => !state.isBytes ? state[key] : bytes(state[key]);

	const btnStart = async(jobName) => {
		jobName = encodeURIComponent(jobName);
		const res = await fetch(settings.apiUrl + '/jobs/start/' + jobName, {method: 'POST'});
		updateJobs();
	}

	async function updateJobs() {
		try {
			jobs = await fetch(settings.apiUrl + '/jobs').then(x => x.json())
			alert = null;
		} catch (ex) {
			jobs = [];
			alert = { class: 'danger', message: ex }			
		}
	}

	onMount(() => {
		updateJobs();
	})

	const mapObject = (obj, handler) => obj && Object.fromEntries(
		Object.entries(obj)
		.map(([key, value]) => [key, handler(key, value)]));	

	useSockets('error', (ev) => alert = { class: 'danger', message: 'Ошибка WebSocket!' });
	useSockets('message', (raw) => {
		const message = JSON.parse(raw);
		if (typeof message.Type === 'undefined') {
			console.log('Unknown message!', message);
			return;
		}
		const { Type, States, Name } = message;
		const final = mapObject(States, (key, data) => ({ State: data}));
		jobs = {...jobs, ...final};
	})
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

	{#each Object.entries(jobs) as [jobName,job] }
		<div class="col-sm-5 m-1">
			<div class="card">
				<div class="card-header">
					Задача: <strong>{jobName}</strong>
				</div>
				<div class="card-body">
					{#if !job.State.inProgress}
						{#if job.Details}
							<div class="mb-3">
								{#if job.Details.nextLaunch}
									<div>
										Следующий запуск:
										<strong>{job.Details.nextLaunch}</strong>
									</div>
								{/if}							
								{#if job.Details.runAfter}
									<div>
										Запускается после:
										<strong>{job.Details.runAfter}</strong>
									</div>
								{/if}
								{#if !job.Details.runAfter && !job.Details.nextLaunch}
									<div>Ручной запуск задачи</div>
								{/if}
								{#if job.Details.copyTo}
									<div>
										Копия архива:
										<strong>{job.Details.copyTo}</strong>
									</div>
								{/if}								
							</div>
						{/if}
						<button class="btn btn-success" on:click={() => btnStart(jobName)}>
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