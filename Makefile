IMAGE = iskorotkov/users-api
TAG = v1.0.1

build:
	docker build -f ./src/WebApi/Dockerfile -t $(IMAGE):$(TAG) .

push:
	docker push $(IMAGE):$(TAG)
