const express = require("express");
const path = require("path");
const fs = require("fs");
const { exec } = require("child_process");

var config = JSON.parse(fs.readFileSync(path.join(__dirname, "config.json")));

const port = config.port;
const passwords = config.passwords;
const baseDir = config.dir;

const app = express();
const router = express.Router({
  strict: true,
});

router.post("*", (req, res) => {
  // req.baseUrl = baseDir + req.baseUrl;

  console.log("Received POST request at: " + req.baseUrl);

  if (!passwords.includes(req.body.password || "")) {
    res.send({ message: "Invalid POST password" });
    return;
  }

  switch (req.body.type || "") {
    case "git":
      console.log("Received valid POST type: git");

      if (req.body.repo === undefined) {
        res.send({ message: '"repo" was undefined!' });
        return;
      }

      res.send({ message: "Validated JSON input, cloning repository." });

      console.log("Deleting ./static/ and cloning repo: " + req.body.repo);

      fs.rmdirSync(path.join(__dirname, baseDir), { recursive: true });
      
      exec(`git clone ${req.body.repo} "${path.join(__dirname, baseDir)}"`);
      break;

    default:
      res.send({ message: "Invalid POST type: " + (req.body.type || "") });
      break;
  }
});

router.get("*", (req, res) => {
  req.baseUrl = baseDir + req.baseUrl;

  console.log("Accepted GET request at: " + req.baseUrl);

  res.sendFile(path.join(__dirname, req.baseUrl));
});

router.patch("*", (req, res) => {
  req.baseUrl = baseDir + req.baseUrl;

  console.log("Denied PATCH request at: " + req.baseUrl);

  res.send({ message: "CDN server doesn't accepted PATCHes" });
});

router.delete("*", (req, res) => {
  req.baseUrl = baseDir + req.baseUrl;

  console.log("Denied DELETE request at: " + req.baseUrl);

  res.send({ message: "CDN server doesn't accepted DELTEs" });
});

app.use(express.json());
app.use("*", router);

app.listen(port, () => {
  console.log("Server listening, hi!");
});
