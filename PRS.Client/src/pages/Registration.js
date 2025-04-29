import { useState, useEffect } from "react";
import axios from "../api/axios";
import { useNavigate } from "react-router-dom";
import { isAdmin } from "../api/auth";
import "../css/Registration.css";
import { useNotification } from "../contexts/NotificationContext";

function Registration({ user }) {
  const { notify } = useNotification();
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
          genders: [...raw.genders, { value: "null", label: "Don't want to specify" }],
          countries: [...raw.countries, { value: "null", label: "Don't want to specify" }]
        });
      })
      .catch(() => {
        notify("Failed to load registration options", "error");
        window.location.href = "/";
      });
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!form.username.trim()) {
      notify("Username is required.", "error");
      return;
    }
    if (!form.email.trim()) {
      notify("Email is required.", "error");
      return;
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(form.email)) {
      notify("Please enter a valid email address.", "error");
      return;
    }
    if (!form.password.trim()) {
      notify("Password is required.", "error");
      return;
    }
    if (!form.gender) {
      notify("Gender is required.", "error");
      return;
    }
    if (!form.country) {
      notify("Country is required.", "error");
      return;
    }
    const date = new Date(form.dateOfBirth);
    if (isNaN(date.getTime())) {
      notify("Please enter a valid date.", "error");
      return;
    }
    const [year, month, day] = form.dateOfBirth.split("-");
    const isRealDate =
      date.getUTCFullYear() === parseInt(year) &&
      date.getUTCMonth() + 1 === parseInt(month) &&
      date.getUTCDate() === parseInt(day);
    if (!isRealDate) {
      notify("Invalid date: This day doesn't exist.", "error");
      return;
    }
    const birthDate = new Date(form.dateOfBirth);
    if (isNaN(birthDate.getTime())) {
      notify("Please enter a valid Date of Birth.", "error");
      return;
    }

    const today = new Date();
    if (birthDate > today) {
      notify("Date of Birth cannot be in the future.", "error");
      return;
    }

    try {
      const payload = {
        ...form,
        gender: form.gender === "null" ? null : parseInt(form.gender),
        country: form.country === "null" ? null : parseInt(form.country),
      };

      await axios.post("/api/register", payload);
      notify("Registration successful!", "success");
      navigate("/login");
    } catch (err) {
      notify(err.response?.data?.errorMessage || "Registration failed.", "error");
    }
  };

  return (
    <div className="registration-page">
      <form onSubmit={handleSubmit} className="registration-form" noValidate>
        <div className="reg-form-group">
          <label>Username:</label>
          <input name="username" value={form.username} onChange={handleChange} />
        </div>
        <div className="reg-form-group">
          <label>Email:</label>
          <input name="email" value={form.email} onChange={handleChange} />
        </div>
        <div className="reg-form-group">
          <label>Password:</label>
          <input name="password" type="password" value={form.password} onChange={handleChange} />
        </div>
        <div className="reg-form-group">
          <label>Gender:</label>
          <select name="gender" value={form.gender} onChange={handleChange}>
            <option value="">Select</option>
            {options.genders.map(g => (
              <option key={g.value} value={g.value}>{g.label}</option>
            ))}
          </select>
        </div>
        <div className="reg-form-group">
          <label>Country:</label>
          <select name="country" value={form.country} onChange={handleChange}>
            <option value="">Select</option>
            {options.countries.map(c => (
              <option key={c.value} value={c.value}>{c.label}</option>
            ))}
          </select>
        </div>
        <div className="reg-form-group">
          <label>Date of Birth:</label>
          <input name="dateOfBirth" type="date" value={form.dateOfBirth} onChange={handleChange} />
        </div>
        <button type="submit" className="reg-submit-btn">Register</button>
      </form>
    </div>
  );
}

export default Registration;
