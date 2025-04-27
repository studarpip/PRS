import { useState, useEffect } from "react";
import axios from "../api/axios";
import { useNavigate } from "react-router-dom";
import { isAdmin } from "../api/auth";
import "../css/Registration.css";

function Registration({ user }) {
  const [options, setOptions] = useState({ genders: [], countries: [] });
  const [form, setForm] = useState({
    username: "",
    email: "",
    password: "",
    gender: "",
    country: "",
    dateOfBirth: ""
  });
  const navigate = useNavigate();

  useEffect(() => {
    if (user) {
      navigate(isAdmin(user) ? "/admin" : "/");
    }
  }, [user, navigate]);

  useEffect(() => {
    axios.get("/api/options/registrationOptions")
      .then(res => {
        const raw = res.data.data;
        setOptions({
          genders: raw.genders,
          countries: raw.countries
        });
      })
      .catch(() => alert("Failed to load registration options"));
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const payload = {
        ...form,
        gender: parseInt(form.gender),
        country: parseInt(form.country),
      };

      await axios.post("/api/register", payload);
      alert("Registration successful!");
      navigate("/login");
    } catch (err) {
      alert(err.response?.data?.message || "Registration failed.");
    }
  };

  return (
    <div className="registration-page">
      <h2>Register</h2>
      <form onSubmit={handleSubmit} className="registration-form">
        <div className="reg-form-group">
          <label>Username:</label>
          <input name="username" value={form.username} onChange={handleChange} required />
        </div>
        <div className="reg-form-group">
          <label>Email:</label>
          <input name="email" type="email" value={form.email} onChange={handleChange} required />
        </div>
        <div className="reg-form-group">
          <label>Password:</label>
          <input name="password" type="password" value={form.password} onChange={handleChange} required />
        </div>
        <div className="reg-form-group">
          <label>Gender:</label>
          <select name="gender" value={form.gender} onChange={handleChange} required>
            <option value="">Select</option>
            {options.genders.map(g => (
              <option key={g.value} value={g.value}>{g.label}</option>
            ))}
          </select>
        </div>
        <div className="reg-form-group">
          <label>Country:</label>
          <select name="country" value={form.country} onChange={handleChange} required>
            <option value="">Select</option>
            {options.countries.map(c => (
              <option key={c.value} value={c.value}>{c.label}</option>
            ))}
          </select>
        </div>
        <div className="reg-form-group">
          <label>Date of Birth:</label>
          <input name="dateOfBirth" type="date" value={form.dateOfBirth} onChange={handleChange} required />
        </div>
        <button type="submit" className="reg-submit-btn">Register</button>
      </form>
    </div>
  );
}

export default Registration;
