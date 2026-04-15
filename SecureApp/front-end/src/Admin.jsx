import { useState } from "react";
import { Navigate } from "react-router-dom";

function Admin(){   
     const role = localStorage.getItem("role");

    if (role !== "Admin") {
        return <Navigate to="/" />;
    }

    return(
    <>
    <h2>Admin panel</h2>
    <ul>
        <li>//user</li>
    </ul>
    </>
    );
}

export default Admin