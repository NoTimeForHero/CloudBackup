import { Fragment } from 'preact';
import { JobViewProps } from './JobView';

export const ButtonsPanel = (props: JobViewProps) => {
  const {job} = props;
  const state = props.realtimeState ?? props.job.State;
  if (state.inProgress) return <Fragment/>;
  return <div>
    {job.Details.jobsAfter &&
        <button className="btn btn-success mr-1"
                onClick={() => props.onJobStart?.call(null, job, true)}
                title="Запустить цепочку заданий">
            <i className="fa fa-forward" aria-hidden="true"/>
        </button>}
    <button className="btn btn-success text-no-wrap"
            onClick={() => props.onJobStart?.call(null, job)}
            title="Запустить задачу">
      <i className="fa fa-play" aria-hidden="true"/>
      <span className="ml-3 d-sm-none d-md-inline">Запустить задачу</span>
    </button>
  </div>
}