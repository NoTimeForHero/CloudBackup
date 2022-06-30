import { Fragment } from 'preact';
import { JobAction, JobViewProps } from './JobView';

export const ButtonsPanel = (props: JobViewProps) => {
  const {job} = props;
  const state = props.realtimeState ?? props.job.State;
  if (state.inProgress) return <Fragment/>;
  return <div>
    <button className="btn btn-warning ml-1"
            title="Редактировать задачу"
            onClick={() => props.onJobAction?.call(null, JobAction.Edit, job)}>
      <i className="fa fa-pencil" aria-hidden="true"/>
    </button>
    <button className="btn btn-primary ml-1"
            title="Создать ярлык на рабочем столе"
            onClick={() => props.onJobAction?.call(null, JobAction.MakeLink, job)}>
      <i className="fa fa-external-link" aria-hidden="true"/>
    </button>
    <button className="btn btn-success text-no-wrap my-1 ml-1"
            onClick={() => props.onJobStart?.call(null, job)}
            title="Запустить задачу">
      <i className="fa fa-play" aria-hidden="true"/>
      <span className="ml-3 d-sm-none d-md-inline">Запустить задачу</span>
    </button>
    {job.Details.jobsAfter &&
        <button className="btn btn-success ml-1"
                onClick={() => props.onJobStart?.call(null, job, true)}
                title="Запустить цепочку заданий">
            <i className="fa fa-forward" aria-hidden="true"/>
        </button>}
  </div>
}