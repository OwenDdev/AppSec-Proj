import { useState } from "react";
import { Link } from "react-router-dom";

function CreateUser() {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [role, setRole] = useState("User"); // ✅ NEW
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");

    async function handleSignup(e) {
        e.preventDefault();

        setError("");
        setMessage("");

        if (!username || !password) {
            setError("Please fill in all fields");
            return;
        }

        try {
            const response = await fetch("https://localhost:7244/api/auth/signup", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    username,
                    password,
                    role 
                })
            });

            if (!response.ok) {
                const text = await response.text();
                setError(text || "Signup failed");
                return;
            }

            const data = await response.json();
            setMessage(data.message || "Account created successfully");
            setUsername("");
            setPassword("");
            setRole("User");

        } catch (err) {
            setError("Server error during signup");
        }
    }

    return (
        <div>
            <h1>Sign Up</h1>

            <form onSubmit={handleSignup}>
                <input
                    type="text"
                    placeholder="Username"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                />

                <br /><br />

                <input
                    type="password"
                    placeholder="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />

                <br /><br />

                {/* 🔥 NEW ROLE SECTION */}
                <p>Admin Privileges:</p>

                <label>
                    <input
                        type="radio"
                        value="User"
                        checked={role === "User"}
                        onChange={(e) => setRole(e.target.value)}
                    />
                    User
                </label>

                <label style={{ marginLeft: "10px" }}>
                    <input
                        type="radio"
                        value="Admin"
                        checked={role === "Admin"}
                        onChange={(e) => setRole(e.target.value)}
                    />
                    Admin
                </label>

                <br /><br />

                <button type="submit">Create Account</button>
            </form>

            {error && <p style={{ color: "red" }}>{error}</p>}
            {message && <p style={{ color: "green" }}>{message}</p>}

        </div>
    );
}

export default CreateUser;