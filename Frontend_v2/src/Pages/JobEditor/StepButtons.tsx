import { BootstrapStyle } from '../../types';

interface StepButtonsProps {
  current: number,
  max: number,
  onStepChange: (direction: number) => void,
  onCancel?: () => void,
  onSave?: () => void,
}

interface Button {
  text: string,
  icon?: string,
  iconAfterText?: boolean,
  onClick?: () => ClickHandler,
  type: BootstrapStyle,
}

type ClickHandler = () => void;

const btnPrev : Button = {text: 'Назад', type: 'primary', icon: 'fa-chevron-left'}
const btnNext : Button = {text: 'Вперёд', type: 'primary', icon: 'fa-chevron-right', iconAfterText: true}
const btnSave : Button = {text: 'Сохранить', type: 'success', icon: 'fa-check-square-o'}
const btnCancel : Button = {text: 'Отменить', type: 'warning', icon: 'fa-sign-out'}
const width = '150px';

const renderButton = (btn: Button, onClick?: ClickHandler) => {
  const iconMargin = btn.iconAfterText ? 'ml-2' : 'mr-2';
  const icon = <i className={`fa ${btn.icon} ${iconMargin}`} aria-hidden="true" />;

  return <button class={`btn btn-${btn.type} mx-1`} style={{width}} onClick={onClick}>
    {!btn.iconAfterText && icon}
    {btn.text}
    {btn.iconAfterText && icon}
  </button>
}

const StepButtons = (props: StepButtonsProps) => {
  const isFirst = props.current === 0;
  const isLast = props.current === props.max - 1;
  const button1 : Button = isFirst ? btnCancel : btnPrev;
  const button2 : Button = isLast ? btnSave : btnNext;
  const handler1 : ClickHandler = isFirst ? () => props.onCancel?.call(null) : () => props.onStepChange(-1);
  const handler2 : ClickHandler = isLast ? () => props.onSave?.call(null) : () => props.onStepChange(1);
  return <>
    {renderButton(button1, handler1)}
    {renderButton(button2, handler2)}
  </>
}

export default StepButtons;