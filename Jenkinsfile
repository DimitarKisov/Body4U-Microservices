pipeline {
  agent any
  stages {
    stage('Verify Branch') {
      steps {
        echo "$GIT_BRANCH"
      }
    }
	stage('Docker Build') {
		steps {
			powershell(script: 'docker-compose build')     
			powershell(script: 'docker images -a')
		}
	 }
	 stage('Run Test Application') {
		steps {
			powershell(script: 'docker-compose up -d')
		}
	 }
	 stage('Stop Test Application') {
		steps {
			powershell(script: 'docker-compose down')
		}
		post {
			success {
				echo "Build successfull!"
			}
			failure {
				echo "Build failed"
			}
		}
	 }
	 stage('Push Images') {
		steps {
			script {
				docker.withRegistry('https://index.docker.io/v1/', 'DockerHub'){
					def image = docker.image("dkisov/body4u-identity-service")
					image.push("${env.BUILD_ID}")
					image.push('latest')
				}
			}
		}
	 }
  }
}