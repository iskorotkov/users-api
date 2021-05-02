IMAGE = iskorotkov/users-api
TAG = v1.0.0

build:
	docker build -f ./src/WebApi/Dockerfile -t $(IMAGE):$(TAG) .

push:
	docker push $(IMAGE):$(TAG)
