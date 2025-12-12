import { useEffect } from "react";
import "../css/Notification.css";
function Notification({ message, onClose }) {
  useEffect(() => {
    const timer = setTimeout(() => {
      onClose();
    }, 4000);

    return () => clearTimeout(timer);
  }, [onClose]);

  return <div className="notification">{message}</div>;
}

export default Notification;
