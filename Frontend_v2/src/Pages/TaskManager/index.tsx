import { useState } from 'preact/compat';
import { getJobs, runJob } from '../../api';
import Alert from '../../Components/Alert';
import { renderError, useAsyncEffect, wrapLoadable } from '../../utils';
import Progress from '../../Components/Progress';
import JobView from './JobView';
import { Job, JobList, JobState } from '../../api/types';
import { useSockets } from '../../socket';

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
    console.log('Запущена задача', job.Name);
    await runJob(job.Name, chain);
    await refreshJobs();
  }

  useAsyncEffect(refreshJobs, []);

  return <div>

    {error && <Alert message={renderError(error)} type={'danger'} />}
    {loading && <Progress />}

    <div className="row">
      {Object.values(jobs).map((job) =>
        <JobView key={job.Name}
                 realtimeState={jobStates[job.Name]}
                 job={job}
                 onJobStart={onJobStart} />)}
    </div>
  </div>
}

export default TaskManager;