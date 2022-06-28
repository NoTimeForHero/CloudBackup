import { cx } from '../utils';

interface ProgressProps {
  className?: string,
  animated?: boolean
  width?: string
}

const Progress = (props?: ProgressProps) => {
  const { width = '100%', animated = true } = props ?? {};
  return <div className={props?.className}>
    <div className="progress">
      <div className={cx("progress-bar","progress-bar-striped", {"progress-bar-animated": animated})}
           style={{width}} />
    </div>
  </div>

}

export default Progress;