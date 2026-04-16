import { useState } from "react";
import { Navigate } from "react-router-dom";
import Navbaruser from "./Navbaruser.jsx";

function Fileupload() {
    const [file, setFile] = useState(null);
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");

    const token = localStorage.getItem("token");

    if (!token) return <Navigate to="/" />;

    async function handleUpload() {
        setError("");
        setMessage("");


        if (!file) {
            setError("Please select a file first");
            return;
        }

        const formData = new FormData();
        formData.append("file", file);
        try{
        const response = await fetch("https://localhost:7244/api/file/upload", {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`
            },
            body: formData
        });

          if (!response.ok) {
                const errorText = await response.text();
                setError(errorText || "Upload failed");
                return;
            }

        const data = await response.json();
        //console.log(data);
        setMessage(data.message || "Upload successful");
        setFile(null);
        }
         catch (err) {
            setError("Server error while uploading file");
        }
    }

    return (
        <>
            <Navbaruser/>
            <h1>User Files</h1>

            <h2>hello User!</h2>

            <ul>
                <li>users files</li>
            </ul>

             {error && <p style={{ color: "red" }}>{error}</p>}
            {message && <p style={{ color: "green" }}>{message}</p>}

            <input type="file" onChange={(e) => setFile(e.target.files[0])} />

            <button onClick={handleUpload}>
                Upload
            </button>
        </>
    );
}

export default Fileupload;