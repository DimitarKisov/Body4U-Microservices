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
					when { branch 'master' }
					def identityImage = docker.image("dkisov/body4u-identity-service")
					identityImage.push("${env.BUILD_ID}")
					identityImage.push('latest')
					
					def adminImage = docker.image("dkisov/body4u-admin-service")
					adminImage.push("${env.BUILD_ID}")
					adminImage.push('latest')
					
					def articleImage = docker.image("dkisov/body4u-article-service")
					articleImage.push("${env.BUILD_ID}")
					articleImage.push('latest')
					
					def articleGatewayImage = docker.image("dkisov/body4u-articlegateway-service")
					articleGatewayImage.push("${env.BUILD_ID}")
					articleGatewayImage.push('latest')
					
					def guideImage = docker.image("dkisov/body4u-guide-service")
					guideImage.push("${env.BUILD_ID}")
					guideImage.push('latest')
					
					def emailSenderImage = docker.image("dkisov/body4u-emailsender-service")
					emailSenderImage.push("${env.BUILD_ID}")
					emailSenderImage.push('latest')
					
					def watchdogImage = docker.image("dkisov/body4u-watchdog-service")
					watchdogImage.push("${env.BUILD_ID}")
					watchdogImage.push('latest')
				}
			}
		}
	 }
  }
}