import { BootstrapStyle } from '../types';
import { cx } from '../utils';

export interface AlertProps {
  message?: string|JSX.Element|undefined,
  type: BootstrapStyle,
  onDismiss?: () => void,
}

const Alert = (props: AlertProps) => {
  const canClose = !!props.onDismiss;
  return <div className="col-12">
    <div className={cx('alert', `alert-${props.type}`)} role="alert">
      {props.message}
      {canClose && <button type="button" className="close" onClick={() => props.onDismiss?.call(null)}>
        <span aria-hidden="true">&times;</span>
      </button>}
    </div>
  </div>
}

export default Alert;