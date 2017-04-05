import React from 'react';
import { storiesOf, action } from '@kadira/storybook';

import FeedList from './index';

storiesOf('Feeds')
  .add('list', () => {
    const list = getFeedsListRelay();
    return <FeedList feeds={list.data.api.nodeManagerData.nugetFeeds} />;
  })
;

const getFeedsListRelay = () => {
  return {
    "data": {
      "api": {
        "nodeManagerData": {
          "nugetFeeds": {
            "edges": [{
              "node": {
                "address": "/opt/packageCache",
                "type": "Private",
                "id": "{\"p\":[{\"f\":\"nodeManagerData\",\"a\":{}},{\"f\":\"nugetFeeds\"}],\"api\":\"ClusterKitNodeApi\",\"id\":1}"
              }, "cursor": null
            }, {
              "node": {
                "address": "http://nuget/",
                "type": "Private",
                "id": "{\"p\":[{\"f\":\"nodeManagerData\",\"a\":{}},{\"f\":\"nugetFeeds\"}],\"api\":\"ClusterKitNodeApi\",\"id\":2}"
              }, "cursor": null
            }]
          }
        }
      }
    }
  }
};
