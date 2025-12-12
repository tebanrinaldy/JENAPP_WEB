import Sidebar from './Sidebar';
import Topbar from './Topbar';
import { Outlet } from 'react-router-dom';
import './MainLayout.css';
import ChatJenApp from "../components/ChatJenApp";

function MainLayout() {
  return (
    <div className="main-layout">
      <div className="sidebar">
        <Sidebar />
      </div>
      <div className="layout-right">
        <Topbar />
        <div className="main-content">
          <Outlet />
        </div>
        <ChatJenApp />
      </div>
    </div>
  );
}

export default MainLayout;
