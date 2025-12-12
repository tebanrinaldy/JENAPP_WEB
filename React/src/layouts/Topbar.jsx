import "./Topbar.css";
import { useNavigate } from "react-router-dom";
import Button from "react-bootstrap/Button";
import { useEffect, useState } from "react";

function Topbar() {
  const navigate = useNavigate();
  const [username, setUsername] = useState("");

  useEffect(() => {
    const userData = sessionStorage.getItem("user");
    if (userData) {
      const user = JSON.parse(userData);
      setUsername(user.username);
    }
  }, []);

  const handlelogout = () => {
    sessionStorage.removeItem("user");
    sessionStorage.removeItem("token");
    alert("Sesión cerrada correctamente");
    navigate("/login");
  };
  return (
    <div className="topbar">
      <div className="topbar-right">
        <span className="topbar-user">👤 {username}</span>
        <Button variant="outline-info" onClick={handlelogout}>
          Salir
        </Button>
      </div>
    </div>
  );
}

export default Topbar;
