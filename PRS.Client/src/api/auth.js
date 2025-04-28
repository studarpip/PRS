import api from "./axios";

export async function login(username, password) {
  try {
    await api.post("/auth/login", { username, password });
    return { success: true };
  } catch (error) {
    const errorMessage = error.response?.data?.errorMessage || "Login failed.";
    return { success: false, errorMessage };
  }
}

export async function logout() {
  await api.post("/auth/logout");
}

export async function getCurrentUser() {
  const res = await api.get("/auth/currentUser");
  return res.data.data;
}

export function isAdmin(user) {
  return user?.role === 0
}
