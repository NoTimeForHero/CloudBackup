import { Fragment } from 'preact';
import bytes from 'bytes';
import Progress from '../../Components/Progress';
import { JobViewProps } from './JobView';

export const ProgressPanel = (props: JobViewProps) => {
  const state = props.job.State;
  if (!state.inProgress) return <Fragment/>;

  const hasBottom = (state.total ?? 0) > 0;
  const current = state.isBytes ? bytes(state.current) : state.current;
  const total = state.isBytes ? bytes(state.total) : state.total;

  return <div className="text-center">
    <h4 style="text-align: center">{state.status}</h4>
    <Progress width={(state.current / state.total * 100) + '%'}/>
    {hasBottom && <div className="mt-1">
      {current} / {total}
    </div>}
  </div>
}