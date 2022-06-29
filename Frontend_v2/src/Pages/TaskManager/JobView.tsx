import { ProgressPanel } from './ProgressPanel';
import { ButtonsPanel } from './ButtonsPanel';
import { Job, JobState } from '../../api/types';

export enum JobAction {
  Edit,
  MakeLink
}

export interface JobViewProps {
  job: Job
  realtimeState?: JobState,
  onJobStart?: (job: Job, chain?: boolean) => void,
  onJobAction?: (action: JobAction, job: Job) => void,
}

const dateFormat = new Intl.DateTimeFormat('ru', {
  day: 'numeric',
  month: 'long'
})
const timeFormat = new Intl.DateTimeFormat('ru', {
  hour: '2-digit',
  minute: '2-digit'
})

const showDate = (nextLaunch: string) => {
  const date = new Date(nextLaunch);
  return dateFormat.format(date) + ' в ' + timeFormat.format(date);
}

const ShowLaunchType = (job: Job) => {
  if (job.Details.nextLaunch) return <div>
    <span className="text-no-wrap">Следующий запуск:&nbsp;</span>
    <span className="text-no-wrap font-weight-bold">{showDate(job.Details.nextLaunch)}</span>
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
      <div className="card-header d-flex align-items-center">
        <div>
          Задача: <strong>{job.Name}</strong>
        </div>
        <div class="ml-auto">
          <button class="btn btn-warning mr-2 px-3 py-1"
                  title="Редактировать задачу"
                  onClick={() => props.onJobAction?.call(null, JobAction.Edit, job)}>
            <i className="fa fa-pencil" aria-hidden="true" />
          </button>
          <button className="btn btn-info px-3 py-1"
                  title="Создать ярлык на рабочем столе"
                  onClick={() => props.onJobAction?.call(null, JobAction.MakeLink, job)}>
            <i className="fa fa-external-link" aria-hidden="true" />
          </button>
        </div>
      </div>
      <div className="card-body">

        <div>ID: <strong>{job.Id}</strong></div>
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