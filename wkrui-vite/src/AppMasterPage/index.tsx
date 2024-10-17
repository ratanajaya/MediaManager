import { useMemo } from 'react';
import { Link, useLocation } from "react-router-dom";

import { Layout, Menu } from 'antd';
import {
  TeamOutlined, PartitionOutlined, TagsOutlined,
  LaptopOutlined, SettingOutlined, IdcardOutlined,
  PieChartOutlined, ProfileOutlined, BarChartOutlined, AppstoreOutlined, ControlOutlined, LogoutOutlined, RadarChartOutlined, BookOutlined, ProductOutlined,
  ConsoleSqlOutlined
} from '@ant-design/icons';
import queryString from 'query-string';

import SelfComps from 'pages/SelfComps';
import Albums from 'pages/Albums';
import Genres from 'pages/Genres';
import Setting from 'pages/Setting';
import _helper from "_utils/_helper";
import _constant from "_utils/_constant";
import { QueryPart } from '_utils/Types';
import FileManagement from 'pages/FileManagement';
import ScCorrection from 'pages/ScCorrection';
import AlbumInfoProvider from '_shared/Contexts/AlbumInfoProvider';
import { useAuth } from '_shared/Contexts/useAuth';
import { SWRConfig } from 'swr';
import cssVariables from '_assets/styles/cssVariables';
import OfflineAlbums from 'pages/OfflineAlbums';
import { ModalProvider } from '_shared/Contexts/ModalProvider';
import QueryEditor from './QueryEditor';
import Querycharts from 'pages/Dashboard/QueryCharts';
import Logs from 'pages/Dashboard/Logs';
import TagGraph from 'pages/Dashboard/TagGraph';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

const { Content, Footer, Sider } = Layout;

const queryClient = new QueryClient();

export default function AppMasterPage() {
  const location = useLocation();

  const { isAuthenticated, fetcherA, logout } = useAuth();
  const pathname = location.pathname.replace('/', '');

  if(!isAuthenticated && !_helper.equalIgnoreCase(pathname, 'Login')){
    window.location.href = "#/login";
  }

  const { page, queryParts } = useMemo(() => {
    const querStr = _helper.nz(queryString.parse(location.search).query as string, "");
    const pageStr = parseInt(_helper.nz(queryString.parse(location.search).page as string, "0"));
    const rowStr = parseInt(_helper.nz(queryString.parse(location.search).row as string, "0")) ;
    const pathStr = _helper.nz(queryString.parse(location.search).path as string, "");
    
    const queryParts: QueryPart = {
      query: querStr,
      page: pageStr,
      row: rowStr,
      path: pathStr
    };

    const page = _helper.equalIgnoreCase(pathname, "Sc") ?
      <SelfComps queryParts={queryParts} /> 
      : _helper.equalIgnoreCase(pathname, "ScCorrection") ? 
      <ScCorrection queryParts={queryParts} />
      : _helper.equalIgnoreCase(pathname, "Albums") ?
      <Albums queryParts={queryParts} />
      : _helper.equalIgnoreCase(pathname, "Artists") ?
      <Genres page={pathname} />
      : _helper.equalIgnoreCase(pathname, "Characters") ?
      <Genres page={pathname} />
      : _helper.equalIgnoreCase(pathname, "Genres") ?
      <Genres page={pathname} />
      : _helper.equalIgnoreCase(pathname, "FileManagement") ?
      <FileManagement path={queryParts.path} type={0} />
      : _helper.equalIgnoreCase(pathname, "OfflineAlbums") ?
      <OfflineAlbums />
      : _helper.equalIgnoreCase(pathname, "Setting") ?
      <Setting />
      : _helper.equalIgnoreCase(pathname, "QueryChart") ?
      <Querycharts />
      : _helper.equalIgnoreCase(pathname, "Logs") ?
      <Logs />
      : _helper.equalIgnoreCase(pathname, "TagGraph") ?
      <TagGraph />
      : <div></div>;

    return {
      page: page,
      queryParts
    };
  }, [pathname, location]);

  const menuItems = [
    {
      key: 'queryeditor',
      label: <QueryEditor query={queryParts.query} />,
      icon: <ConsoleSqlOutlined />,
    },
    {
      key: 'albums',
      label: <Link to="/albums">Albums</Link>,
      icon: <BookOutlined />,
    },
    {
      key: 'sub0',
      label: 'Queries',
      icon: <TagsOutlined />,
      children: [
        {
          key: 'artists',
          label: <Link to="/artists">Artists</Link>,
          icon: <TeamOutlined />,
        },
        {
          key: 'characters',
          label: <Link to="/characters">Characters</Link>,
          icon: <IdcardOutlined />,
        },
        {
          key: 'genres',
          label: <Link to="/genres">Genres</Link>,
          icon: <PartitionOutlined />,
        }
      ]
    },
    {
      key: 'sub1',
      label: 'Dashboard',
      icon: <BarChartOutlined />,
      children: [
        {
          key: 'querychart',
          label: <Link to="/querychart">Query Chart</Link>,
          icon: <PieChartOutlined />,
        },
        {
          key: 'logs',
          label: <Link to="/logs">Logs</Link>,
          icon: <ProfileOutlined />,
        },
        {
          key: 'taggraph',
          label: <Link to="/taggraph">Tag Graph</Link>,
          icon: <RadarChartOutlined />,
        }
      ]
    },
  ];

  const privateOnlyMenuItems = [
    {
      key: 'sub2',
      label: 'Self Comp',
      icon: <LaptopOutlined />,
      children: [
        {
          key: 'sc',
          label: <Link to="/sc">Views</Link>,
          icon: <AppstoreOutlined />,
        },
        {
          key: 'sccorrection',
          label: <Link to="/sccorrection">Correction</Link>,
          icon: <ControlOutlined />,
        }
      ]
    },
    {
      key: 'offlinealbums',
      label: <Link to="/offlinealbums">Offline Albums</Link>,
      icon: <ProductOutlined />,
    },
  ]

  const settingMenuItem = {
      key: 'setting',
      label: <Link to="/setting">Setting</Link>,
      icon: <SettingOutlined />,
    };

  const allMenuItems = [
    ...menuItems, 
    ...(!_constant.isPublic ? privateOnlyMenuItems : []), 
    settingMenuItem
  ];

  return (
    <SWRConfig value={{ 
      revalidateOnFocus: false, 
      fetcher: fetcherA
    }}>
      <QueryClientProvider client={queryClient}>
        <AlbumInfoProvider>
          <ModalProvider>
            <Layout style={{ minHeight: '100vh' }}>
              <Sider breakpoint="lg" collapsedWidth="0">
                <div className="logo"></div>
                <Menu 
                  theme='dark'
                  selectedKeys={[pathname]}
                  defaultOpenKeys={["sub0"]} 
                  mode="inline" 
                  items={allMenuItems}
                />
                <div style={{ borderBottom:"1px solid grey", width:"100%", marginBottom:"10px" }}></div>
                <div 
                  onClick={() => logout()}
                  style={{
                    color: cssVariables.textSub,
                    position:'fixed', 
                    bottom:0,
                    width:'100%',
                    padding:'24px',
                    cursor: 'pointer'
                  }}>
                  <LogoutOutlined /><span style={{marginLeft:'10px'}}>Logout</span>
                </div>
              </Sider>
              <Layout className="site-layout">
                <Content>
                  <div className="site-layout-background">
                    {page}
                  </div>
                </Content>
                <Footer style={{ textAlign: 'center' }}></Footer>
              </Layout>
            </Layout>
          </ModalProvider>
        </AlbumInfoProvider>
      </QueryClientProvider>
    </SWRConfig>
  );
}