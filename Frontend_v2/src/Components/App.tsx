import { ExtendedWindow } from '../types';
import { createContext, useEffect, useState } from 'preact/compat';
import HealthCheck from './HealthCheck';
import { cx } from '../utils';
import TaskManager from '../Pages/TaskManager';
import Logs from '../Pages/Logs';
import Plugins from '../Pages/Plugins';
import SettingsPage from '../Pages/Settings';

export interface MenuItem {
  name: string,
  title: string,
  component: () => JSX.Element
}

interface AppContext {
  setBody: (body: JSX.Element|undefined) => void,
}

export const menu: MenuItem[] = [
  {name: 'tasks', title: 'Задачи', component: () => <TaskManager/>},
  {name: 'logs', title: 'Логи', component: () => <Logs/>},
  {name: 'plugins', title: 'Плагины', component: () => <Plugins/>},
  {name: 'settings', title: 'Настройки', component: () => <SettingsPage/>},
]

export const appContext = createContext<AppContext>({
  setBody: () => {},
});

const App = () => {
  const settings = (window as ExtendedWindow).settings!;

  const [activeMenu, setActiveMenu] = useState<MenuItem>(menu[0]);
  const [component, setComponent] = useState<JSX.Element>();

  const setMenu = (item: MenuItem) => (ev: MouseEvent) => {
    setActiveMenu(item);
    // Позволяет выйти из модального окна, например создания новой задачи?
    setComponent(item.component());
  };

  useEffect(() => {
    const hash = document.location.hash;
    if (!hash) return;
    const needed = menu.find(x => `#${x.name}` === hash);
    if (!needed) return;
    setActiveMenu(needed);
  }, []);

  useEffect(() => setComponent(activeMenu.component()), [activeMenu]);

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

      <appContext.Provider value={{setBody: setComponent}}>
        {component}
      </appContext.Provider>
    </main>
  </>
}

const WrappedApp = () => <App/>;

export default WrappedApp;