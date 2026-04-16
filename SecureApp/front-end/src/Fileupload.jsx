import { useState,  useEffect } from "react";
import { Navigate } from "react-router-dom";
import Navbaruser from "./Navbaruser.jsx";

function Fileupload() {
    const [file, setFile] = useState(null);
    const [files, setFiles] = useState([]);
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");

    const token = localStorage.getItem("token");

    useEffect(() => {
        fetchFiles();
    }, []);

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
        fetchFiles();
        }
         catch (err) {
            setError("Server error while uploading file");
        }
    }

    async function fetchFiles() {
        try {
            const response = await fetch("https://localhost:7244/api/file/myfiles", {
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (!response.ok) {
                setError("Failed to load files");
            return;
        }

        const data = await response.json();
        setFiles(data);
        } catch (err) {
            setError("Error fetching files");
        }
    }

    async function handleDownload(id) {
    try {
        const response = await fetch(`https://localhost:7244/api/file/download/${id}`, {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            setError("Download failed");
            return;
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);

        const a = document.createElement("a");
        a.href = url;
        a.download = "file";
        document.body.appendChild(a);
        a.click();
        a.remove();
        }
        catch {
            setError("Error downloading file");
        }
    }

    async function handleDelete(id) {
    try {
        const response = await fetch(`https://localhost:7244/api/file/${id}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            setError("Delete failed");
            return;
        }

        setMessage("File deleted");
        fetchFiles();
        } catch {
            setError("Error deleting file");
        }
    }
    return (
        <>
            <Navbaruser/>
            <h1>User Files</h1>

            <h2>hello User!</h2>

            <ul>
            {files.length === 0 ? (
            <li>No files uploaded yet</li>
                ) : (
                    files.map((f) => (
                        <li key={f.id}>
                            {f.fileName}
                            {f.fileName}

                            <button onClick={() => handleDownload(f.id)}>
                                Download
                            </button>
                            <button onClick={() => handleDelete(f.id)}>
                                Delete
                            </button>
                        </li>
                    ))
                )}
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