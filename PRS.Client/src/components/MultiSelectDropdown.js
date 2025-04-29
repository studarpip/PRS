import { useState, useEffect, useRef } from "react";
import "../css/MultiSelectDropdown.css"

function MultiSelectDropdown({ options, selectedValues, onChange, placeholder }) {
  const [showDropdown, setShowDropdown] = useState(false);
  const dropdownRef = useRef(null);

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target)) {
        setShowDropdown(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  const toggleOption = (value) => {
    if (selectedValues.includes(value)) {
      onChange(selectedValues.filter(v => v !== value));
    } else {
      onChange([...selectedValues, value]);
    }
  };

  return (
    <div className="multi-select-container" ref={dropdownRef}>
      <div
        onClick={() => setShowDropdown(!showDropdown)}
        className="multi-select-display"
      >
        {selectedValues.length > 0
          ? options
            .filter(opt => selectedValues.includes(opt.value))
            .map(opt => opt.label)
            .join(", ")
          : placeholder || "Select..."}
      </div>

      {showDropdown && (
        <div className="multi-select-dropdown">
          {options.map(opt => (
            <div key={opt.value} className="multi-select-option">
              <label className="multi-select-label">
                <input
                  type="checkbox"
                  checked={selectedValues.includes(opt.value)}
                  onChange={() => toggleOption(opt.value)}
                  className="multi-select-checkbox"
                />
                {opt.label}
              </label>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default MultiSelectDropdown;
