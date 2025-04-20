import { useEffect, useState } from "react";
import { getCurrentUser } from "../api/auth";

function Home() {
    const [user, setUser] = useState(null);

    useEffect(() => {
        getCurrentUser()
            .then(setUser)
            .catch(() => { });
    }, []);

    return (
        <div>
            <h2>Welcome to the home page</h2>

            {user ? (
                <div style={{ marginTop: "1rem" }}>
                    <p><strong>Username:</strong> {user.username}</p>
                    <p><strong>Role:</strong> {user.role}</p>
                    <p><strong>User ID:</strong> {user.userId}</p>
                </div>
            ) : (
                <p>Loading user info...</p>
            )}
        </div>
    );
}

export default Home;
