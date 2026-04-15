import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import Navbar from "./Navbar";

function Admin() {
    const role = localStorage.getItem("role");
    const token = localStorage.getItem("token");

    const [users, setUsers] = useState([]);

    if (role !== "Admin") {
        return <Navigate to="/" />;
    }

    async function fetchUsers() {
        try {
            const response = await fetch("https://localhost:7244/api/auth/users", {
                headers: {
                    Authorization: `Bearer ${token}`
                }
            });

            const data = await response.json();
            setUsers(data);
        } catch (err) {
            console.log("Error loading users:", err);
        }
    }

    async function deleteUser(id) {
        try {
            await fetch(`https://localhost:7244/api/auth/users/${id}`, {
                method: "DELETE",
                headers: {
                    Authorization: `Bearer ${token}`
                }
            });

            // refresh list
            fetchUsers();
        } catch (err) {
            console.log("Delete error:", err);
        }
    }

    useEffect(() => {
        fetchUsers();
    }, []);

    return (
        <>
             <Navbar />
            <h2>Admin Dashboard</h2>

            <table border="1" cellPadding="10">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Username</th>
                        <th>Role</th>
                        <th>Action</th>
                    </tr>
                </thead>

                <tbody>
                    {users.map(user => (
                        <tr key={user.id}>
                            <td>{user.id}</td>
                            <td>{user.username}</td>
                            <td>{user.role}</td>
                            <td>
                                <button onClick={() => deleteUser(user.id)}>
                                    Delete
                                </button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </>
    );
}

export default Admin;