import { useState } from 'preact/compat';
import { getJobs, runJob } from '../../api';
import Alert from '../../Components/Alert';
import { renderError, useAsyncEffect, wrapLoadable } from '../../utils';
import Progress from '../../Components/Progress';
import JobView from './JobView';
import { Job, JobList } from '../../api/types';

const TaskManager = () => {

  const [error, setError] = useState<any>();
  const [loading, setLoading] = useState(false);
  const [jobs, setJobs] = useState<JobList>({});

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
      {Object.values(jobs).map((job) => <JobView key={job.Name} job={job} onJobStart={onJobStart} />)}
    </div>
  </div>
}

export default TaskManager;