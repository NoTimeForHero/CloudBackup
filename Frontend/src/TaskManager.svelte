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

	const btnStart = async(jobName) => {
		jobName = encodeURIComponent(jobName);
		const res = await fetch(settings.apiUrl + '/jobs/start/' + jobName, {method: 'POST'});
		document.location.reload();
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

	let socketClient = undefined;

	function makeSocketClient() {
		const rawUrl = new URL(settings.apiUrl);
		const socket = new WebSocket(`ws://${rawUrl.host}/ws-status`);
		socket.onmessage = (ev) => {
			const message = JSON.parse(ev.data);
			if (!message.Type) {
				console.log('Unknown message!', message);
				return;
			}
			const { Type, States, Name } = message;
			jobs = {...jobs, ...States};
		}
		socket.onerror = (ev) => {
			console.error('WebSocket Error!', ev);
			alert = { class: 'danger', message: 'Ошибка WebSocket!' }			
		}
		const onClose = () => {
			console.log('WebSocket closed...');
			socket.close();
		}
		return { close: onClose };
	}

	onMount(()=> {
		updateJobs();
		socketClient = makeSocketClient();
	});

	onDestroy(()=> {
		socketClient.close();
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

	{#each Object.entries(jobs) as [jobName,job] }
		<div class="col-sm-5 m-2">
			<div class="card">
				<div class="card-header">
					Задача: <strong>{jobName}</strong>
				</div>
				<div class="card-body">
					{#if !job.inProgress}
						<button class="btn btn-success" on:click={() => btnStart(jobName)}>
							<i class="fa fa-play" aria-hidden="true"></i>&nbsp;Запустить задачу							
						</button>
					{:else}
					<div class="center">
						<h4 style="text-align: center">{job.status}</h4>
						<div class="progress">
							<div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar"
								aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: {calcPercent(job)}%"></div>
						</div>		
						{#if job.total > 0}
						<div class="mt-1">
							{isBytes(job, 'current')} / {isBytes(job, 'total')}
						</div>						
						{/if}
					</div>						
					{/if}
				</div>
			</div>
		</div>
	{/each}
</div>