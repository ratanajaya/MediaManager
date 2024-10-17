import { HashRouter, Route, Routes, Navigate } from 'react-router-dom';

import '_assets/styles/App.scss';
import '_assets/styles/AntdOverride.scss';
import '_assets/styles/App.css';

import AppMasterPage from 'src/AppMasterPage';
import { ConfigProvider } from 'antd';
import { theme } from 'antd';
import Sandbox from 'pages/Sandbox';
import SettingProvider from '_shared/Contexts/SettingProvider';
import Login from 'pages/Login';
import { NotifProvider } from '_shared/Contexts/NotifProvider';
import 'react-bootstrap-range-slider/dist/react-bootstrap-range-slider.css';

const appMasterPagePaths = [
  "/sc", "/sccorrection", "/albums", "/artists", "/characters", "/genres", 
  "/filemanagement", "/offlinealbums", "/setting", "/querychart", "/logs", 
  "/taggraph"
];

export default function App() {
  return (
    <ConfigProvider theme={{
      algorithm: theme.darkAlgorithm,
    }}>
      <SettingProvider>
        <NotifProvider>
          <HashRouter>
            <Routes>
              <Route path="/" element={<Navigate to="/genres" />} />
              <Route path="/sandbox" element={<Sandbox />} />
              <Route path="/login" element={<Login />} />
              {appMasterPagePaths.map(path => (
                <Route key={path} path={path} element={<AppMasterPage />} />
              ))}
            </Routes>
          </HashRouter>
        </NotifProvider>
      </SettingProvider>
    </ConfigProvider>
  );
}