import { Settings } from '../types';
import { FC, useMemo, useState } from 'preact/compat';
import { cx } from '../utils';
import HealthCheck from './HealthCheck';
import Logs from '../Pages/Logs';
import Plugins from '../Pages/Plugins';
import SettingsPage from '../Pages/Settings';
import TaskManager from '../Pages/TaskManager';

interface MenuItem {
  name: string,
  title: string,
  component: () => JSX.Element
}

const menu : MenuItem[] = [
  { name: 'tasks', title: 'Задачи', component: () => <TaskManager />},
  { name: 'logs', title: 'Логи', component: () => <Logs />},
  { name: 'plugins', title: 'Плагины', component: () => <Plugins /> },
  { name: 'settings', title: 'Настройки', component: () => <SettingsPage />},
]

interface AppProps {
  settings: Settings
}

export const App : FC<AppProps> = (props) => {
  const { settings } = props;

  const [activeMenu, setActiveMenu] = useState<MenuItem>(menu[0]);
  const component = useMemo(() => activeMenu.component(), [activeMenu]);

  const setMenu = (item: MenuItem) => (ev: MouseEvent) => {
    setActiveMenu(item);
  };

  return <>
    <main className="container">

      <nav className="navbar navbar-light mb-4">
        <div className="navbar-brand">
          <HealthCheck />
          <span className="ml-3">{settings.appName}</span>
        </div>
        <div id="navbarNavAltMarkup">
          {menu.map((item) => (
            <a key={item.name}
               href={`#${item.name}`}
               className={cx("nav-item nav-link", {active: activeMenu.name === item.name})}
              onClick={setMenu(item)}>
              {item.title}
            </a>
          ))}
        </div>
      </nav>

      {component}
    </main>
  </>
}
export default App;