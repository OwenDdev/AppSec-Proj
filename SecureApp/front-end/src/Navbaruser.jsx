import { useNavigate } from "react-router-dom";

function Navbaruser() {
    const navigate = useNavigate();
    


    function logout() {
        localStorage.removeItem("token");
        localStorage.removeItem("role");
        localStorage.removeItem("refreshToken");
        navigate("/");
    }

    return (
        <nav style={{ display: "flex", gap: "10px" }}>
            <button onClick={() => navigate("/user")}>User</button>
            <button onClick={logout}>Logout</button>
        </nav>
    );
}

export default Navbaruser;