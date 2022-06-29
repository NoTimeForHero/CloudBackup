import { useContext, useState } from 'preact/compat';
import { getJobs, runJob } from '../../api';
import Alert from '../../Components/Alert';
import { renderError, useAsyncEffect, wrapLoadable } from '../../utils';
import Progress from '../../Components/Progress';
import JobView, { JobAction } from './JobView';
import { Job, JobList, JobState } from '../../api/types';
import { useSockets } from '../../socket';
import { appContext } from '../../Components/App';
import JobEditor from '../JobEditor';
import LinkTool from '../LinkTool';

enum MessageType {
  Started,
  Completed,
  ProgressUpdated
}

const TaskManager = () => {

  const [error, setError] = useState<any>();
  const [loading, setLoading] = useState(false);
  const [jobs, setJobs] = useState<JobList>({});
  const [jobStates, setJobStates] = useState<Record<string,JobState>>({});
  const context = useContext(appContext);

  useSockets('message', (raw: string) => {
    const message = JSON.parse(raw);
    if (typeof message.Type === 'undefined') {
      console.log('Unknown message!', message);
      return;
    }
    const { Type, States } = message;
    if (Type == MessageType.ProgressUpdated && States) setJobStates(States);
    else refreshJobs();
  });

  const refreshJobs = () => wrapLoadable(
    async() => setJobs(await getJobs()),
    { setLoading, setError }
  );

  const onJobStart = async(job: Job, chain?: boolean) => {
    console.log('Запущена задача', job.Id, '->', job.Name);
    await runJob(job.Id, chain);
    await refreshJobs();
  }

  const onJobAction = (action: JobAction, job: Job) => {
    switch (action) {
      case JobAction.Edit:
        context.setBody(<JobEditor edited={job} />)
        break;
      case JobAction.MakeLink:
        context.setBody(<LinkTool job={job} />)
        break;
      default:
        throw new Error(`Unknown action: ${action}!`)
    }
  }

  const onNewJob = () => context.setBody(<JobEditor />);

  useAsyncEffect(refreshJobs, []);

  return <div>

    {error && <Alert message={renderError(error)} type={'danger'} />}
    {loading && <Progress />}

    <div className="d-flex justify-content-center mb-3">
      <button class="btn btn-primary" onClick={onNewJob}>
        <i className="fa fa-plus-square mr-2" aria-hidden="true" />
        Создать новую задачу
      </button>
    </div>

    <div className="row">
      {Object.values(jobs).map((job) =>
        <JobView key={job.Id}
                 realtimeState={jobStates[job.Id]}
                 job={job}
                 onJobAction={onJobAction}
                 onJobStart={onJobStart} />)}
    </div>
  </div>
}

export default TaskManager;