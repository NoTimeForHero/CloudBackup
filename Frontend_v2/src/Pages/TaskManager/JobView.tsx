import { ProgressPanel } from './ProgressPanel';
import { ButtonsPanel } from './ButtonsPanel';
import { Job } from '../../api/types';

export interface JobViewProps {
  onJobStart?: (job: Job, chain?: boolean) => void,
  job: Job
}

const ShowLaunchType = (job: Job) => {
  if (job.Details.nextLaunch) return <div>
    <span className="text-no-wrap">Следующий запуск:&nbsp;</span>
    <span className="text-no-wrap font-weight-bold">{job.Details.nextLaunch}</span>
  </div>
  if (job.Details.runAfter) return <div>
    Запускается после:&nbsp;
    <strong>{job.Details.runAfter}</strong>
  </div>
  return <div>Ручной запуск</div>;
}

const JobView = (props: JobViewProps) => {
  const { job } = props;
  const { Details } = job;
  return <div className="col-sm-5 m-1 mb-2 mt-2">
    <div className="card">
      <div className="card-header">
        Задача: <strong>{job.Name}</strong>
      </div>
      <div className="card-body">

        {Details.description}
        {Details.description && <hr/>}
        {ShowLaunchType(job)}
        {Details.copyTo && <div>
            Копия архива:&nbsp;
            <strong>{job.Details.copyTo}</strong>
        </div>}

        <hr/>

        <div>
          <ButtonsPanel {...props} />
          <ProgressPanel {...props} />
        </div>

      </div>
    </div>
  </div>
}

export default JobView;