import { useState } from "react";
import { useDispatch } from "react-redux";
import { useNavigate } from "react-router-dom";
import { postRegister } from "../../services/registerService";
import { Button, Link } from "@mui/material";
import Alert from "@mui/material/Alert";
import JustValueTextField from "../../generalComponents/JustValueTextField";
import { StyledTitle } from "../../generalComponents/styled/Title.styled";
import { StyledCenteredColumn } from "../../generalComponents/styled/CenteredColumn.styled";
import { getInfo } from "../../services/registerService";
import { BACKGROUND, GREEN_BUTTON, LOGIN_URL } from "../../constants";
import { StyledLinearProgress } from "../../generalComponents/styled/LinearProgress.styled";
import ErrorMessage from "../../generalComponents/ErrorMessage";
import SubmitButton from "../../generalComponents/SubmitButton";

function RegisterPage() {
  const [userName, setUsername] = useState();
  const [password, setPassword] = useState();
  const [cPassword, setCPassword] = useState();
  const [name, setName] = useState();
  const [surname, setSurname] = useState();
  const [email, setEmail] = useState();
  const [phoneNumber, setPhoneNumber] = useState();

  const [errorMsg, setErrorMsg] = useState();
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [infoAlert, setInfoAlert] = useState(false);
  const [information, setInformation] = useState("");

  const dispatch = useDispatch();
  const navigate = useNavigate();

  function checkValidation() {
    if (password !== cPassword) {
      setErrorMsg("Passwords do not match!");
    } else if (
      !(
        userName &&
        password &&
        cPassword &&
        name &&
        surname &&
        email &&
        phoneNumber
      )
    ) {
      setErrorMsg("All fields are required");
    } else {
      setErrorMsg(null);
    }
  }

  function handleData() {
    const dataObject = {
      userName,
      password,
      name,
      surname,
      email,
      phoneNumber,
    };
    if (
      password === cPassword &&
      userName &&
      password &&
      cPassword &&
      name &&
      surname &&
      email &&
      phoneNumber
    )
      dispatch(
        postRegister(dataObject, navigate, setErrorMsg, setLoading, setSuccess)
      );
  }

  return (
    <>
      <StyledCenteredColumn
        style={{ padding: "5% 35%", backgroundColor: BACKGROUND }}
      >
        <StyledTitle>TiGets</StyledTitle>
        <JustValueTextField label="userName" setValue={setUsername} />
        <JustValueTextField
          label="password"
          type="password"
          setValue={setPassword}
          setRequiredErrMsg={setErrorMsg}
        />
        <JustValueTextField
          label="confirm password"
          type="password"
          setValue={setCPassword}
          setRequiredErrMsg={setErrorMsg}
        />
        <JustValueTextField
          label="name"
          setValue={setName}
          setRequiredErrMsg={setErrorMsg}
        />
        <JustValueTextField
          label="surname"
          setValue={setSurname}
          setRequiredErrMsg={setErrorMsg}
        />
        <JustValueTextField
          label="email"
          setValue={setEmail}
          setRequiredErrMsg={setErrorMsg}
        />
        <JustValueTextField
          label="phone number"
          setValue={setPhoneNumber}
          setRequiredErrMsg={setErrorMsg}
        />

        {errorMsg && <ErrorMessage text={errorMsg} />}

        <SubmitButton
          onClick={() => {
            checkValidation();
            if (!errorMsg) {
              handleData();
            }
          }}
          text={"Register"}
        />
        {loading && <StyledLinearProgress />}
        {success && <StyledLinearProgress color="success" />}
        <Link
          style={{ textAlign: "center", color: GREEN_BUTTON }}
          onClick={() => {
            navigate(LOGIN_URL);
          }}
        >
          Already have and account? Log in.
        </Link>
      </StyledCenteredColumn>
    </>
  );
}

export default RegisterPage;
