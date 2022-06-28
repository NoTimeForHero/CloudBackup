import { Fragment } from 'preact';
import { JobViewProps } from './JobView';

export const ButtonsPanel = (props: JobViewProps) => {
  const {job} = props;
  if (job.State.inProgress) return <Fragment/>;
  return <div>
    {job.Details.jobsAfter &&
        <button className="btn btn-success mr-1"
                onClick={() => props.onJobStart?.call(null, job, true)}
                title="Запустить цепочку заданий">
            <i className="fa fa-forward" aria-hidden="true"/>
        </button>}
    <button className="btn btn-success"
            onClick={() => props.onJobStart?.call(null, job)}
            title="Запустить задачу">
      <i className="fa fa-play mr-3" aria-hidden="true"/>
      Запустить задачу
    </button>
  </div>
}