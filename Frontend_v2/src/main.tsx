import { render } from 'preact'
import App from './Components/App'
import './index.css'
import { createSockets } from './socket';
import { ExtendedWindow, Settings } from './types';

const initialize = async() => {
  const settings: Settings = await fetch('/settings.json').then(x => x.json());
  (window as ExtendedWindow).settings = settings;
  createSockets(settings, true);
  render(<App settings={settings}/>, document.getElementById('app')!)
}
// noinspection JSIgnoredPromiseFromCall
initialize();